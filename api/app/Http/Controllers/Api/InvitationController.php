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
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Auth;

use Illuminate\Support\Str;

class InvitationController extends Controller
{
    public function create(Album $album, InvitationCreateRequest $request): JsonResponse
    {
        $expiresAt = $request->input('expiresAt');
        $timeLimit = $request->input('timeLimit');

        if ($expiresAt)
            $expiresDate = $expiresAt;

        else if ($timeLimit)
            $expiresDate = now()->addMinutes((int)$timeLimit);

        $invitation = Invitation::create([
            'expires_at' => $expiresDate ?? null,
            'album_id' => $album->id,
            'join_limit' => $request->joinLimit ?? null,
            'link' => Str::random(8)
        ]);

        return response()->json(['invitation' => InvitationResource::make($invitation)], 201);
    }

    public function album(Invitation $invitation): JsonResponse
    {
        $invitation->failOnExpires();
        $invitation->load(['album', 'album.user']);
        return response()->json(InvitationResource::make($invitation));
    }

    public function join(Invitation $invitation): JsonResponse
    {
        $invitation->failOnExpires();
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
        return response()->json(null, 204);
    }

    public function destroy(Invitation $invitation): JsonResponse
    {
        $user = Auth::user();
        if ($user->id !== $invitation->album->user_id)
            throw new ForbiddenException();

        $invitation->delete();
        return response()->json(null, 204);
    }
}
