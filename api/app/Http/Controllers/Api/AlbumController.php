<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Album\AlbumCreateRequest;
use App\Http\Requests\Api\Album\AlbumUpdateRequest;
use App\Http\Resources\AlbumResource;
use App\Models\Album;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;

class AlbumController extends Controller
{
    public function index()
    {
        $user = Auth::user();
        return response([
            'own'        => AlbumResource::collection($user->albums),
            'accessible' => AlbumResource::collection($user->albumsViaAccess),
        ]);
    }

    public function create(AlbumCreateRequest $request)
    {
        $user = Auth::user();
        $name      = $request->input('name');
        $inputPath = $request->input('path');

        // Проверка, существует ли альбом с таким путём и возвращаем его если да
        $existAlbum = Album
            ::where('path', $inputPath)
            ->where('user_id', $user->id)
            ->first();
        if ($existAlbum)
            return response([
                'message' => 'Album with this path already exists',
                'album' => $existAlbum
            ], 409);

        // Проверка, существует ли альбом с таким именем и модифицируем имя если да
        $nameCounter = 1;
        $originalName = $name;
        while (Album
            ::where('name', $name)
            ->where('user_id', $user->id)
            ->exists()
        ) $name = $originalName . ++$nameCounter;

        // Создание нового альбома с уникальным именем
        $newAlbum = Album::create([
            'name' => $name,
            'path' => $inputPath,
            'user_id' => $user->id
        ]);

        // Создание пустой директории
        $path = "users/$user->login/albums/$newAlbum->id";
        Storage::deleteDirectory($path);
        Storage::  makeDirectory($path);

        // Формирование ответа
        return response([
            'message' => 'Album created',
            'album' => $newAlbum
        ], 201);
    }

    public function edit(AlbumUpdateRequest $request, $albumId)
    {
        $album = Album::findOrFail($albumId)->update($request->all()); // TODO: Проверять валидность и брать валидные
        return response(['album' => $album]);
    }

    public function destroy($albumId)
    {
        $album = Album::findOrFail($albumId);
        $user = Auth::user();
        Storage::deleteDirectory("users/$user->login/albums/$albumId");
        $album->delete();
        return response(['message' => 'Album deleted']);
    }
}
