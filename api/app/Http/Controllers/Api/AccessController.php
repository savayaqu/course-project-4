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
        $albumIds = Album
            ::where('user_id', Auth::id())
            ->pluck('id')
            ->toArray();

        // Поиск всех выданных доступов и созданных приглашений на СВОИ альбомы
        $accesses    = AlbumAccess::whereIn('album_id', $albumIds)->get();
        $invitations = Invitation ::whereIn('album_id', $albumIds)->get();

        return response([
            'accesses'    => AlbumAccessResource::collection($accesses),
            'invitations' => InvitationResource::collection($invitations),
        ]);
    }

    public function destroy(Album $album, User $userId = null)
    {
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
