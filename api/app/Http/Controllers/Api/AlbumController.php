<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Album\AlbumCreateRequest;
use App\Http\Requests\Api\Album\AlbumUpdateRequest;
use App\Http\Resources\AlbumResource;
use App\Models\Album;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;

class AlbumController extends Controller
{
    public function index(): JsonResponse
    {
        $user = Auth::user();
        return response()->json([
            'own' => AlbumResource::collection(
                $user
                ->albums()
                ->withCount([
                    'pictures',
                    'invitations',
                    'usersViaAccess',
                ])
                ->with([
                    'pictures' => fn ($query) => $query->limit(4),
                ])
                ->get()
            ),
            'accessible' => AlbumResource::collection(
                $user
                ->albumsViaAccess()
                ->withCount([
                    'pictures',
                ])
                ->with([
                    'pictures' => fn ($query) => $query->limit(4),
                    'user',
                ])
                ->get()
            ),
        ]);
    }

    public function create(AlbumCreateRequest $request): JsonResponse
    {
        $user = Auth::user();
        $name      = $request->input('name');
        $inputPath = $request->input('path');

        // Проверка, существует ли альбом с таким путём и возвращаем его если да
        if ($inputPath) {
            $existAlbum = Album
                ::where('path', $inputPath)
                ->where('user_id', $user->id)
                ->first();
            if ($existAlbum)
                return response()->json([
                    'message' => 'Album with this path already exists',
                    'album' => AlbumResource::make($existAlbum)
                ], 409);
        }

        // Проверка, существует ли альбом с таким именем и модифицируем имя счётчиком если да
        $nameCounter = 1;
        $originalName = $name;
        while (Album
            ::where('name', $name)
            ->where('user_id', $user->id)
            ->exists()
        ) $name = "$originalName (". ++$nameCounter .')';

        // Создание нового альбома с уникальным именем
        $newAlbum = Album::create([
            'name' => $name,
            'path' => $inputPath,
            'user_id' => $user->id,
        ]);

        // Создание пустой директории
        $path = $newAlbum->getPath();
        Storage::deleteDirectory($path);
        Storage::  makeDirectory($path);

        // Формирование ответа
        return response()->json(['album' => AlbumResource::make($newAlbum)], 201);
    }

    public function show(Album $album): JsonResponse
    {
        $user = Auth::user();
        if ($album->user_id === $user?->id)
            $relations = ['invitations', 'usersViaAccess'];
        else
            $relations = ['user'];

        $album->load([
            'pictures' => fn ($query) => $query->limit(4),
            ...$relations,
        ])->loadCount(['pictures']);
        return response()->json(['album' => AlbumResource::make($album)]);
    }

    public function update(AlbumUpdateRequest $request, Album $album): JsonResponse
    {
        $album->update($request->validated());
        return response()->json(['album' => AlbumResource::make($album)]);
    }

    public function destroy(Album $album): JsonResponse
    {
        Storage::deleteDirectory($album->getPath());
        $album->delete();
        return response()->json(['message' => 'Album deleted']);
    }
}
