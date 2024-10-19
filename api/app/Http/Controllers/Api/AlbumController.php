<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\Album;
use App\Models\Picture;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Str;

class AlbumController extends Controller
{
    public function index(Request $request)
    {
        $user = Auth::user();
        $albums = Album::where('user_id', $user->id)->get();
        if($albums->isEmpty())
        {
            throw new ApiException('Альбомы не найдены', 404);
        }
        return response($albums)->setStatusCode(200);
    }
    public function showPictures(Request $request, $album_id)
    {
        $user = Auth::user();
        $album = Album::where('name', $album_id)->first();
        if(!$album)
        {
            throw new ApiException('Альбом не найден', 404);
        }
        $pictures = Picture::with('album')->where('album_id', $album->id)->where('user_id', $user->id)->get();
        if($pictures->isEmpty())
        {
            throw new ApiException('Картинки в альбоме не найдены', 404);
        }
        return response($pictures)->setStatusCode(200);
    }
    public function create(Request $request)
    {
        $user = Auth::user();
        $name = $request->input('name');
        $input_path = $request->input('path');
        if($exist_album = Album::where('name', $name)->where('user_id', $user->id)->exists())
        {
            throw new ApiException('Данный альбом уже существует', 409);
        }
        else
        {
            Album::create([
               'name' => $name,
               'path' => Hash::make(Str::random(60)),
               'user_id' => $user->id
            ]);
            $current_album = Album::where('name', $name)->where('user_id', $user->id)->first();
            $path = $user->login.'/albums/'.$current_album->id;

            if(Storage::exists($path) && !$exist_album)
            {
                Storage::deleteDirectory($path);
                //return response('папка удалена');
            }
            $current_album->path = $path.'/'.$input_path;
            Storage::createDirectory($current_album->path);
            return  response(['message' => 'Папка создана', $current_album])->setStatusCode(201);
        }

    }

    public function destroy(Request $request, $album_id) {

        $album = Album::where('id', $album_id)->first();
        if(!$album) {
            throw new ApiException('Альбом не найден', 404);
        }
        //Проверка что папка принадлежит текущему пользователю
        $user = Auth::user();
        $files = Picture::where('album_id', $album->id)->get();
        if(Album::where('id', $album_id)->where('user_id',$user->id)->first()) {
            Storage::deleteDirectory($album->path);
            foreach ($files as $file) {
                Picture::where('id', $file->id)->delete();
            }
            Album::where('id', $album_id)->delete();
            return response("Альбом удален")->setStatusCode(200);
        }
        else {
            throw new ApiException('Forbidden for you', 403);
        }


    }
}
