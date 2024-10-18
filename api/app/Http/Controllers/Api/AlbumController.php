<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\Album;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;

class AlbumController extends Controller
{
    public function create(Request $request)
    {
        $user = Auth::user();
     $name = $request->input('name');
     $path = '/albums/' . $name;
     if(Storage::exists('albums/' . $name))
     {
         throw  new ApiException('Данная папка уже существует', 409);
     }
     Storage::createDirectory($path);
     $album = Album::create(['name' => $name, 'path' => $path, 'user_id' => $user->id]);
     return  response(['Папка создана', $album])->setStatusCode(201);
    }

    public function destroy(Request $request, $album_name) {

        $album = Album::where('name', $album_name)->first();
        if(!$album) {
            throw new ApiException('Альбом не найден', 404);
        }
        //Проверка что папка принадлежит текущему пользователю
        $user = Auth::user();
        if(Album::where('name', $album_name)->where('user_id',$user->id)) {
            Storage::deleteDirectory($album->path);
            Album::destroy($album_name);
            return response("Альбом удален")->setStatusCode(204);
        }
        else {
            throw new ApiException('Forbidden for you', 403);
        }


    }
}
