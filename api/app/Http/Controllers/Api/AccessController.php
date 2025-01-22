<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Exceptions\Api\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Resources\AlbumResource;
use App\Models\Album;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Auth;

class AccessController extends Controller
{
    public function index(): JsonResponse
    {
        $user = Auth::user();
        $albumsOwned = $user->albums()->with(['invitations', 'usersViaAccess'])->get();
        $albums = $albumsOwned->filter(fn($album) =>
            $album->invitations   ->isNotEmpty() ||
            $album->usersViaAccess->isNotEmpty()
        );
        return response()->json(['albums' => AlbumResource::collection($albums)]);
    }

    public function destroy(Album $album, User $user = null): JsonResponse
    {
        if ($user === null)
            $user = Auth::user();

        if ($user->id === $album->user_id)
            throw new ApiException('You are owner', 409);

        //else if ($user->id !== $album->user_id)
        //    throw new ForbiddenException();

        $album->usersViaAccess()->detach($user->id);
        return response()->json(null, 204);
    }
}
