<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Picture\PictureCreateRequest;
use App\Http\Resources\PictureResource;
use App\Models\Album;
use App\Models\Picture;
use Carbon\Carbon;
use Exception;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Facades\Validator;
use Intervention\Image\Laravel\Facades\Image as Intervention;
use Symfony\Component\HttpFoundation\BinaryFileResponse;

class PictureController extends Controller
{
    public function index(Request $request, Album $album): JsonResponse
    {
        // Получаем ID тегов
        $tagIds = null;
        $tagsString = $request->input('tags');
        if ($tagsString)
            $tagIds = explode(',', $tagsString);

        $sortBy = $request->query('sort', 'date');   // Сортировка по полю (по умолчанию дата)
        $orderBy  = $request->has('reverse') ? 'desc' : 'asc'; // Направление сортировки

        $user = Auth::user();

        // Проверка валидации сортировки
        $allowedSortFields = ['date', 'name', 'width', 'height', 'size'];
        if (!in_array($sortBy, $allowedSortFields))
            throw new ApiException('Sort must be of the following types: ' . join(', ', $allowedSortFields), 400);


        // Фильтрация по заданным параметрам
        $query = Picture::with('tags')
            ->where('album_id', $album->id)
            ->orderBy($sortBy, $orderBy);

        if ($tagIds) {
            $query->whereHas('tags', function ($query) use ($tagIds) {
                $query->whereIn('id', $tagIds); // Фильтруем по любому из указанных тегов
            }, '=', count($tagIds));
        }

        $pictures = $query->get();

        $sign = $album->getSign($user);

        return response()->json([
            'sign' => $sign,
            'pictures' => PictureResource::collection($pictures),
        ]);
    }

    public function create(PictureCreateRequest $request, Album $album): JsonResponse
    {
        $user = Auth::user();
        $files = $request->file('pictures');
        $pathToSave = Picture::getPathStatic($user->id, $album->id);
        $errored = [];
        $successful = [];

        // Обрабатываем файлы по одному
        foreach ($files as $file) {
            $filename = $file->getClientOriginalName();
            try {
                // Валидация mimes файла и пропускаем если не в разрешённых
                $validator = Validator::make(['file' => $file], [
                    'file' => 'mimes:jpeg,jpg,png,gif', // TODO: Управлять настройками
                ]);
                if ($validator->fails()) {
                    $errored[] = [
                        'name'    => $filename,
                        'message' => 'Validation failed',
                        'errors'  => $validator->errors()->toArray()['file'],
                    ];
                    continue;
                }

                // Проверка, существует ли картинка с таким хешем и пропускаем если да
                $tmpPath = $file->getRealPath();
                $pictureHash = hash_file('xxh3', $tmpPath);
                $existedPictureByHash = Picture
                    ::where('album_id', $album->id)
                    ->where('hash', $pictureHash)
                    ->first();
                if ($existedPictureByHash) {
                    $errored[] = [
                        'name' => $filename,
                        'message' => 'Already exist with this hash',
                        'picture' => PictureResource::make($existedPictureByHash),
                    ];
                    continue;
                }
                // Проверка, существует ли картинка с таким именем и модифицируем имя счётчиком если да
                $counter = 1;
                $extension      = pathinfo($filename, PATHINFO_EXTENSION);
                $nameWithoutExt = pathinfo($filename, PATHINFO_FILENAME);
                $filenameValid = $filename;
                while (Picture
                    ::where('album_id', $album->id)
                    ->where('name', $filenameValid)
                    ->exists()
                ) {
                    $counter++;
                    $filenameValid = "$nameWithoutExt ($counter).$extension";
                }

                // Получение размеров, пропускаем если не получилось
                try {
                    $sizes = getimagesize($tmpPath);
                }
                catch (Exception $e) {
                    $errored[] = [
                        'name' => $filename,
                        'message' => "No sizes",
                    ];
                    continue;
                }

                // Создаём запись в БД
                $pictureDB = Picture::create([
                    'name' => $filenameValid,
                    'hash' => $pictureHash,
                    'date' => Carbon::createFromTimestamp(filemtime($tmpPath)), // TODO: Надо наверное в API дату отправлять
                    'size' => $file->getSize(),
                    'width'  => $sizes[0],
                    'height' => $sizes[1],
                    'album_id' => $album->id,
                ]);

                // Сохраняем в ФС
                $file->storeAs($pathToSave, $filenameValid);

                $successful[] = $pictureDB;
            } catch (Exception $e) {
                // Что-то пошло не так
                $errored[] = [
                    'name' => $filename,
                    'message' => "Something going wrong",
                ];
            }
        }
        return response()->json([
            'sign' => $album->getSign($user),
            'successful' => $successful,
            'errored' => $errored,
        ]);
    }

    public function info(Album $album, Picture $picture): JsonResponse
    {
        $picture->load('tags');
        $user = Auth::user();
        $sign = $album->getSign($user);
        return response()->json([
            'sign' => $sign,
            'picture' => PictureResource::make($picture),
        ]);
    }

    public function thumbnail(Request $request, $albumId, $pictureId, $orientation, $size): BinaryFileResponse|JsonResponse|RedirectResponse
    {
        $ownerId = $request->attributes->get('ownerId');
        $orientation = strtolower($orientation);

        // Проверка существования в ФС с исходными данными
        $thumbPath = Picture::getPathThumbStatic($ownerId, $albumId, $pictureId, $orientation, $size);

        if (!Storage::exists($thumbPath)) {
            // Проверка запрашиваемого размера и редирект, если не прошло
            $askedSize = $size;
            $allowedSizes = [144, 240, 360, 480, 720, 1080]; // TODO: Управлять настройками
            $allowSize = false;
            foreach ($allowedSizes as $allowedSize) {
                if ($size <= $allowedSize) {
                    $size = $allowedSize;
                    $allowSize = true;
                    break;
                }
            }
            if (!$allowSize) $size = $allowedSizes[count($allowedSizes)-1];
            if ($askedSize != $size) return redirect()
                ->route('thumbnail',
                    [$albumId, $pictureId, $orientation, $size, 'sign' => $request->query('sign')])
                ->header('Cache-Control', ['max-age=86400', 'private']);

            try {
                $picture = Picture
                    ::where('album_id', $albumId)
                    ->where('id', $pictureId)
                    ->firstOrFail();
            }
            catch (Exception $e) {
                return response()->json(['message' => 'Picture not found'], 404)
                    ->header('Cache-Control', ['max-age=86400', 'private']);
            }

            // Редирект если изображение квадратное
            if ($picture->width === $picture->height) return redirect()
                ->route('thumbnail',
                    [$albumId, $pictureId, $orientation, $size, 'sign' => $request->query('sign')])
                ->header('Cache-Control', ['max-age=86400', 'private']);

            // Создание превью
            $picturePath = $picture->getPath($ownerId);
            $thumb = Intervention::read(Storage::get($picturePath));

            switch ($orientation) {
                case 'w':
                    $thumb->scaleDown(width: $size);
                    break;
                case 'h':
                    $thumb->scaleDown(height: $size);
                    break;
                default:
                    $thumb->coverDown($size, $size);
                    break;
            }
            $dirname = pathinfo($thumbPath, PATHINFO_DIRNAME);
            if (!Storage::exists($dirname))
                Storage::makeDirectory($dirname);

            $thumb
                ->toJpeg(90)
                ->save(Storage::path($thumbPath));
        }
        return response()->file(Storage::path($thumbPath), ['Cache-Control' => ['max-age=86400', 'private']]);
    }

    public function original($albumId, Picture $picture, Request $request): BinaryFileResponse
    {
        $ownerId = $request->attributes->get('ownerId');
        $path = Storage::path($picture->getPath($ownerId));
        return response()->file($path);
    }

    public function download($albumId, Picture $picture, Request $request): BinaryFileResponse
    {
        $ownerId = $request->attributes->get('ownerId');
        $path = Storage::path($picture->getPath($ownerId));
        return response()->download($path, $picture->name);
    }

    public function destroy(Album $album, Picture $picture): JsonResponse
    {
        Storage::delete($picture->getPath($album->user_id));
        $picture->delete();
        return response()->json(null, 204);
    }
}
