<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Exceptions\Api\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Invitation\InvitationCreateRequest;
use App\Http\Resources\InvitationResource;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Invitation;
use Illuminate\Support\Facades\Auth;

use Illuminate\Support\Str;

class InvitationController extends Controller
{
    public function create(Album $album, InvitationCreateRequest $request)
    {
        if ($request->expiresAt)
            $expiresDate = $request->expiresAt;

        else if ($request->timeLimit)
            $expiresDate = now()->addMinutes((int)$request->timeLimit);

        $invitation = Invitation::create([
            'expires_at' => $expiresDate ?? null,
            'album_id' => $album->id,
            'join_limit' => $request->joinLimit ?? null,
            'link' => Str::random(8)
        ]);

        return response(['invitationCode' => $invitation->link], 201);
    }

    public function album(Invitation $invitation)
    {
        $invitation->checkExpires();
        return response(InvitationResource::make($invitation->with(['album', 'album.user'])->first()));
    }

    public function join(Invitation $invitation)
    {
        $invitation->checkExpires();
        $user = Auth::user();

        if ($user->id === $invitation->album->user_id)
            throw new ApiException('You are owner', 409);

        if (AlbumAccess
            ::where('user_id', $user->id)
            ->where('album_id', $invitation->album_id)
            ->first()
        ) throw new ApiException('Already accessible', 409);

        AlbumAccess::create([
            'user_id' => $user->id,
            'album_id' => $invitation->album_id
        ]);

        if ($invitation->join_limit !== null) {
            $invitation->join_limit -= 1;

            if ($invitation->join_limit > 0)
                $invitation->save();
            else
                $invitation->delete();
        }
        return response(null, 204);
    }

    public function destroy(Invitation $invitation)
    {
        $user = Auth::user();
        if ($user->id !== $invitation->album->user_id)
            throw new ForbiddenException();

        $invitation->delete();
        return response(null, 204);
    }
}
