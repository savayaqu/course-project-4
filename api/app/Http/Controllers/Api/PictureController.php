<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\Album;
use App\Models\Picture;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;
use Intervention\Image\Drivers\Gd\Driver;
use Intervention\Image\ImageManager;
class PictureController extends Controller
{
    public function thumbnail($album_id, $picture_id, $size, Request $request)
    {
        $album = Album::findOrFail($album_id);

        $picture = Picture::where('id', $picture_id)->where('album_id', $album_id)->first();
        if (!$picture) {
            throw new ApiException('Not found', 404);
        }

        $sign = Album::checkSign($album_id, $request->query('sign'));
        if (!$sign) {
            throw new ApiException('Forbidden', 403);
        }
        $orientation = $picture->height > $picture->width ? 'h' : 'w';

        $thumbPath = "users/$sign/thumbs/$picture->name-$orientation$size.jpg";
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
            $thumbPath = "users/$sign/thumbs/$picture->name-$orientation$size.jpg";
            if (!Storage::exists($thumbPath)) {
                // Создание превью
                //if (!isset($image)) $image = Image::getByHash($album_id, $picture_id);

                //$imagePath = 'images'. $image->album->path . $image->name;
                $imagePath = 'users/'.$sign.'/albums/'.$album_id.'/pictures/'.$picture->name;

                $manager = new ImageManager(new Driver());
                $thumb = $manager->read(Storage::get($imagePath));

                if ($orientation == 'w')
                    $thumb->scale(width: $size);
                else
                    $thumb->scale(height: $size);

                if (!Storage::exists('users/'.$sign.'/thumbs'))
                    Storage::makeDirectory('users/'.$sign.'/thumbs');

                $thumb->toJpeg(90)->save(Storage::path($thumbPath));
            }
        }
        return response()->file(Storage::path($thumbPath));
    }

    public function destroy($album_id, $picture_id)
    {
        $user = Auth::user();
        $album = Album::find($album_id);
        if(!$album)
        {
            throw new ApiException('Альбом не найден', 404);
        }

        $picture = Picture::where('id', $picture_id)->where('album_id', $album_id)->first();
        if (!$picture) {
            throw new ApiException('Картинка не найдена', 404);
        }
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
        $picture = Picture::where('album_id', $album_id)->where('id', $picture_id)->first();
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

        //$check = Album::checkSign($sign);
        return response()->json([ 'sign' => $sign,'pictures' => $pictures]);
    }

    public function original($album_id, $picture_id, Request $request)
    {

        $album = Album::findOrFail($album_id);
        $picture = Picture::findOrFail($picture_id)->where('album_id', $album_id)->first();
        $sign = Album::checkSign($album_id, $request->query('sign'));
        if (!$sign)
        {
            throw new ApiException('Доступ запрещён', 403);
        }
        $path = Storage::path('users/'.$sign.'/albums/'.$album->id.'/pictures/'.$picture->name);
        return response()->file($path);
    }

    public function download($album_id, $picture_id, Request $request)
    {

        $album = Album::findOrFail($album_id)->first();
        $picture = Picture::findOrFail($picture_id);
        $sign = Album::checkSign($album_id, $request->query('sign'));
       if (!$sign)
       {
           throw new ApiException('Доступ запрещён', 403);
       }
        $path = Storage::path('users/'.$sign.'/albums/'.$album->id.'/pictures/'.$picture->name);
        return response()->download($path, $picture->name);
    }

    public function create(Request $request, $album_id)
    {
        $user = Auth::user();
        $files = $request->file('pictures');
        $album = Album::where('id', $album_id)->where('user_id', $user->id)->first();

        $path = 'users/'.$user->login.'/albums/'.$album->id.'/pictures/';
        $responses = [];
        foreach ($files as $file) {
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
        }
        return response($responses);
    }
}
