<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Picture\PictureCreateRequest;
use App\Models\Album;
use App\Models\Picture;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;
use Intervention\Image\Drivers\Gd\Driver;
use Intervention\Image\ImageManager;
use Illuminate\Validation\ValidationException;
class PictureController extends Controller
{
    public function thumbnail($album_id, $picture_id, $size, Request $request)
    {
        Album::findOrFail($album_id);

        $picture = Picture::findOrFail($picture_id);

        $orientation = $picture->height > $picture->width ? 'h' : 'w';

        $thumbPath = "users/$request->login/thumbs/$picture->name-$orientation$size.jpg";
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
            if ($askedSize != $size) throw new ApiException('Allowed sizes: 144, 240, 360, 480, 720, 1080', 400);


            // Проверка наличия превью в файлах x2
            $thumbPath = "users/$request->login/thumbs/$picture->name-$orientation$size.jpg";
            if (!Storage::exists($thumbPath)) {
                // Создание превью
                $imagePath = 'users/'.$request->login.'/albums/'.$album_id.'/pictures/'.$picture->name;
                $manager = new ImageManager(new Driver());
                $thumb = $manager->read(Storage::get($imagePath));
                if ($orientation == 'w')
                    $thumb->scale(width: $size);
                else
                    $thumb->scale(height: $size);

                if (!Storage::exists('users/'.$request->login.'/thumbs'))
                    Storage::makeDirectory('users/'.$request->login.'/thumbs');

                $thumb->toJpeg(90)->save(Storage::path($thumbPath));
            }
        }
        return response()->file(Storage::path($thumbPath));
    }

    public function destroy($album_id, $picture_id)
    {
        $user = Auth::user();
        Album::findOrFail($album_id);
        $picture = Picture::findOrFail($picture_id)->where('album_id', $album_id)->first();
        $path = 'users/' . $user->login . '/albums/' . $album_id . '/pictures/' . $picture->name;
        if(Storage::exists($path))
        {
           Storage::delete($path);
        }
        $picture->delete();
        return response()->json()->setStatusCode(204);

    }

    public function info($album_id, $picture_id)
    {
        $picture = Picture::findOrFail($picture_id)->where('album_id', $album_id)->first();
        return response()->json($picture);
    }

    public function index(Request $request, $album_id)
    {
        $user = Auth::user();
        $album = Album::findOrFail($album_id);
        $pictures = Picture::without('album')->where('album_id', $album->id)->get();
        if($pictures->isEmpty())
        {
            throw new ApiException('Картинки в альбоме не найдены', 404);
        }
        $sign = Album::getSign($user, $album_id);
        return response()->json([ 'sign' => $sign,'pictures' => $pictures]);
    }

    public function original($album_id, $picture_id, Request $request)
    {
        $album = Album::findOrFail($album_id);
        $picture = Picture::findOrFail($picture_id)->where('album_id', $album_id)->first();
        $path = Storage::path('users/'.$request->login.'/albums/'.$album->id.'/pictures/'.$picture->name);
        return response()->file($path);
    }

    public function download($album_id, $picture_id, Request $request)
    {

        $album = Album::findOrFail($album_id)->first();
        $picture = Picture::findOrFail($picture_id);
        $path = Storage::path('users/'.$request->login.'/albums/'.$album->id.'/pictures/'.$picture->name);
        return response()->download($path, $picture->name);
    }

    public function create(PictureCreateRequest $request, $album_id)
    {
        $user = Auth::user();
        $files = $request->file('pictures');
        $album = Album::findOrFail($album_id);

        $path = 'users/'.$user->login.'/albums/'.$album->id.'/pictures/';
        $responses = [
            'successful' => [],
            'errored' => [],
        ];

        // Обрабатываем файлы по одному
        foreach ($files as $index => $file) {
            try {
                // Попробуем валидацию для каждого файла
                $request->validate([
                    'pictures.' . $index => 'required|file|mimes:jpeg,jpg,png,gif',
                ]);

                $filename = $file->getClientOriginalName();
                $pictureHash = hash('xxh3', Storage::path($path.$filename));

                $picture = Picture::where('album_id', $album_id)->where('hash', $pictureHash)->first();
                if ($picture) {
                    $responses['errored'][] = [
                        'name' => $filename,
                        'message' => 'already exist in this album'
                    ];
                    continue;
                }

                $file->storeAs($path, $filename);
                $sizes = getimagesize(Storage::path($path.$filename));

                $pictureDB = Picture::create([
                    'name' => $filename,
                    'hash' => $pictureHash,
                    'date' => Carbon::createFromTimestamp(Storage::lastModified($path.$filename)),
                    'size' => Storage::size($path.$filename),
                    'width' => $sizes[0],
                    'height' => $sizes[1],
                    'album_id' => $album_id,
                ]);
                $responses['successful'][] = $pictureDB;
            } catch (ValidationException $e) {
                // Получаем ошибки и добавляем их в список ошибок
                $errors = $e->errors();
                $fileErrors = $errors['pictures.' . $index] ?? []; // Получаем ошибки для текущего файла

                foreach ($fileErrors as $message) {
                    $responses['errored'][] = [
                        'name' => $file->getClientOriginalName(),
                        'message' => $message,
                    ];
                }
            }
        }

        return response($responses);
    }


}
