<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\Album;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Storage;

class AlbumController extends Controller
{
    public function create(Request $request)
    {
     $name = $request->input('name');
     $path = '/albums/' . $name;
     if(Storage::exists('albums/' . $name))
     {
         throw  new ApiException('Данная папка уже существует', 409);
     }
     Storage::createDirectory($path);
     Album::create(['name' => $name, 'path' => $path]);
    }

    public function destroy(Request $request, $album_name) {
        $album = Album::where('name', $album_name)->first();
        if(!$album) {
            throw new ApiException('Альбом не найден', 404);
        }
        Storage::delete($album->path);
        Album::destroy($album_name);

    }
}
