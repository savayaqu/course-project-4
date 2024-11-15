<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Resources\AlbumResource;
use App\Models\Album;
use App\Models\User;
use Illuminate\Support\Facades\Auth;

class AccessController extends Controller
{
    public function index()
    {
        $user = Auth::user();
        $albumsOwned = $user->albums()->with(['invitations', 'usersViaAccess'])->get();
        $albums = $albumsOwned->filter(fn($album) =>
            $album->invitations   ->isNotEmpty() ||
            $album->usersViaAccess->isNotEmpty()
        );
        return response(['albums' => AlbumResource::collection($albums)]);
    }

    public function destroy(Album $album, User $user = null)
    {
        if ($user === null)
            $user = Auth::user();

        else if ($user->id !== $album->user_id)
            throw new ForbiddenException();

        $album->usersViaAccess()->detach($user->id);
        return response(null, 204);
    }
}
