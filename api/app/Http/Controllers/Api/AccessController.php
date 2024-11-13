<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\NotFoundException;
use App\Http\Controllers\Controller;
use App\Http\Resources\AlbumAccessResource;
use App\Http\Resources\InvitationResource;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Invitation;
use App\Models\User;
use Illuminate\Support\Facades\Auth;

class AccessController extends Controller
{
    public function index()
    {
        // TODO: Сделать не (accesses -> album) & (invites -> album), а albums -> (invites & accesses)
        $albums = Album::with(['albumAccesses', 'invitations'])->where('user_id', Auth::id())->get();
        $result = $albums->map(function ($album) {
           return [
               'accesses' => AlbumAccessResource::collection($album->albumAccesses),
               'invitations' => InvitationResource::collection($album->invitations),
           ] ;
        });
       return response()->json($result , 200);
    }

    public function destroy(Album $album, $userId = null)
    {
        if ($userId)
            User::findOrFailCustom($userId);

        $albumAccess = AlbumAccess
            ::where('album_id', $album->id)
            ->where('user_id', $userId ?? Auth::id())
            ->first();

        if(!$albumAccess)
            throw new NotFoundException(AlbumAccess::class);

        $albumAccess->delete();

        return response(null, 204);
    }
}
