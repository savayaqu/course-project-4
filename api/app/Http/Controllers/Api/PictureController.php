<?php

namespace App\Http\Controllers\Api;

use App\Cacheables\SpaceInfo;
use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Picture\PictureCreateRequest;
use App\Http\Requests\Api\Picture\PictureUpdateRequest;
use App\Http\Resources\PictureResource;
use App\Models\Album;
use App\Models\Picture;
use Exception;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Facades\Validator;
use Intervention\Image\Laravel\Facades\Image as Intervention;
use phpDocumentor\Reflection\File;
use Symfony\Component\HttpFoundation\BinaryFileResponse;
use Symfony\Component\Mime\MimeTypes;

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

        // Админ может делать сортировку по жалобам
        if ($user->role->code === 'admin') {
            if($sortBy === 'complaints') {
                $allowedSortFields[] = 'complaints';
            }
        }

        if (!in_array($sortBy, $allowedSortFields))
            throw new ApiException('Sort must be of the following types: ' . join(', ', $allowedSortFields), 400);


        // Фильтрация по заданным параметрам
        $query = Picture::with('tags')
            ->withCount('complaints')
            ->where('album_id', $album->id);

        // Сортировка
        if ($sortBy === 'complaints') {
            $query->orderBy('complaints_count', $orderBy); // Сортировка по количеству жалоб
        } else {
            $query->orderBy($sortBy, $orderBy); // Сортировка по другим полям
        }

        if ($tagIds) {
            $query->whereHas('tags', function ($query) use ($tagIds) {
                $query->whereIn('id', $tagIds); // Фильтруем по любому из указанных тегов
            }, '=', count($tagIds));
        }

        $limit = intval($request->limit);
        if (!$limit)
            $limit = 30;

        $picturesPage = $query->paginate($limit);

        $sign = $album->getSign($user);

        return response()->json([
            'sign'     => $sign,
            'page'     => $picturesPage->currentPage(),
            'limit'    => $picturesPage->perPage(),
            'total'    => $picturesPage->total(),
            'pictures' => PictureResource::collection($picturesPage->items()),
        ]);
    }

    public function store(PictureCreateRequest $request, Album $album): JsonResponse
    {
        $user = Auth::user();
        $pictures = $request->pictures;
        $pathToSave = Picture::getPathStatic($user->id, $album->id);

        $spaceInfo = SpaceInfo::getCached();
        if ($spaceInfo->usedPercent >= config('settings.upload_disable_percentage'))
            throw new ApiException('Server in read-only mode', 400);

        // Получаем сколько сейчас весят пользовательские картинки и какой лимит по загрузкам
        $currentStorageSize = $user->quotaUsed();
        $maxStorageSize = $user->quotaTotal();

        // Массивы для ответа
        $errored    = [];
        $successful = [];

        // Обрабатываем файлы по одному
        foreach ($pictures as $key => $pictureInRequests) {
            try {
                $file = $request->file("pictures.$key.file");
                $date = $pictureInRequests['date'] ?? now();
                $filename = $pictureInRequests['name'] ?? $file->getClientOriginalName();

                logger("\$req filename: $filename");

                $extReq = pathinfo($filename, PATHINFO_EXTENSION);
                $extReal = $file->guessExtension() ?? $file->extension();
                if ($extReal != null && $extReal != $extReq)
                    $filename .= ".$extReal";

                logger("\$upd filename: $filename");

                // Валидация mimes файла и пропускаем если не в разрешённых
                $validator = Validator::make($pictureInRequests, [
                    'file' => 'mimes:' . implode(',', config('settings.allowed_upload_mimes')),
                ]);
                if ($validator->fails()) {
                    $errored[] = [
                        'name'    => $filename,
                        'message' => $validator->errors()->toArray()['file'][0],
                    ];
                    continue;
                }

                // Проверяем, превышает ли размер лимит
                $filesize = $file->getSize();
                if ($currentStorageSize + $filesize >= $maxStorageSize) {
                    $errored[] = [
                        'name'    => $filename,
                        'message' => 'Storage limit reached',
                    ];
                    continue;
                }

                // Проверка, существует ли картинка с таким хешем и пропускаем если да
                $tmpPath = $file->getRealPath();
                $hash = hash_file('xxh3', $tmpPath);
                $existedPictureByHash = Picture
                    ::where('album_id', $album->id)
                    ->where('hash', $hash)
                    ->first();
                if ($existedPictureByHash) {
                    $errored[] = [
                        'name' => $filename,
                        'message' => 'Already exist with this hash',
                        'picture' => PictureResource::make($existedPictureByHash),
                    ];
                    continue;
                }

                if (!preg_match('~^[^/\\\\:*?"<>|]+$~u', $filename))
                    $filename = $hash . ".$extReal";

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

                logger("\$filenameValid: $filenameValid");

                // Получение размеров, пропускаем если не получилось
                try {
                    $imageSizes = getimagesize($tmpPath);
                }
                catch (Exception) {
                    $errored[] = [
                        'name' => $filename,
                        'message' => 'No sizes',
                    ];
                    continue;
                }

                // Создаём запись в БД
                $pictureDB = Picture::create([
                    'name'      => $filenameValid,
                    'hash'      => $hash,
                    'date'      => $date,
                    'size'      => $filesize,
                    'width'     => $imageSizes[0],
                    'height'    => $imageSizes[1],
                    'album_id'  => $album->id,
                ]);

                // Сохраняем в ФС
                $file->storeAs($pathToSave, $filenameValid);

                // Добавление текущего занятого пространства
                $currentStorageSize += $filesize;

                // Запись как успешного в ответ
                $successful[] = $pictureDB;
            } catch (Exception $ex) {
                // Что-то пошло не так
                $errored[] = [
                    'name' => $filename,
                    'message' => 'Something going wrong'
                    . config('app.debug')
                        ? ("\n" . $ex->getMessage())
                        : '',
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

    public function update(PictureUpdateRequest $request, Album $album, Picture $picture): JsonResponse
    {
        $path = $picture->getPath($album->user_id);
        $name = $request->name;

        $mimeTypes = new MimeTypes();
        $mimeType = $mimeTypes->guessMimeType(Storage::path($path));
        $extReal = $mimeTypes->getExtensions($mimeType)[0] ?? '';

        $extReq = pathinfo($request->name, PATHINFO_EXTENSION);

        if ($extReal != null && $extReal != $extReq)
            $name .= ".$extReal";

        if ($picture->name == $name)
            return response()->json(['picture' => PictureResource::make($picture)]);

        $counter = 1;
        $extension      = pathinfo($name, PATHINFO_EXTENSION);
        $nameWithoutExt = pathinfo($name, PATHINFO_FILENAME);
        $nameValid = $name;
        while (Picture
            ::where('album_id', $album->id)
            ->where('name', $nameValid)
            ->exists()
        ) {
            $counter++;
            $nameValid = "$nameWithoutExt ($counter).$extension";
        }

        $picture->update(['name' => $nameValid]);
        Storage::move($path, $picture->getPath($album->user_id));

        return response()->json(['picture' => PictureResource::make($picture)]);
    }

    public function thumbnail(Request $request, $albumId, $pictureId, $orientation, $size): BinaryFileResponse|JsonResponse|RedirectResponse
    {
        $allowedSizes = config('settings.allowed_preview_sizes');

        $ownerId = $request->attributes->get('ownerId');
        $orientation = strtolower($orientation);

        // Проверка существования в ФС с исходными данными
        $thumbPath = Picture::getPathThumbStatic($ownerId, $albumId, $pictureId, $orientation, $size);

        if (!Storage::exists($thumbPath)) {
            // Проверка запрашиваемого размера и редирект, если не прошло
            $askedSize = $size;
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
            catch (Exception) {
                return response()->json(['message' => 'Picture not found'], 404)
                    ->header('Cache-Control', ['max-age=86400', 'private']);
            }

            // Редирект если изображение квадратное
            if ($picture->width === $picture->height  && $orientation !== 'q') return redirect()
                ->route('thumbnail',
                    [$albumId, $pictureId, 'q', $size, 'sign' => $request->query('sign')])
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
        return response()->file(Storage::path($thumbPath), ['Cache-Control' => 'max-age=86400, private']);
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
