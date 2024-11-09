<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Invitation\InvitationCreateRequest;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Invitation;
use Illuminate\Support\Facades\Auth;
use Illuminate\Http\Request;

use Illuminate\Support\Str;

class InvitationController extends Controller
{
    public function destroy($albumId)
    {
        //Проверка принадлежности альбома пользователю
        $user = Auth::user();
        $album = Album::find($albumId);
        if (!$album) throw new ApiException('Not found', 404);
        if($album->user_id != $user->id)
        {
            throw new ApiException('Forbidden', 403);
        }
        $invitation = Invitation::where('album_id', $albumId)->first();
        $invitation->delete();
        return response()->json()->setStatusCode(204);
    }
    public function album($code)
    {
        $user = Auth::user();
        $invitation = Invitation::where('link', $code)->first();
        if(!$invitation) throw new ApiException('Приглашение недействительно', 404);
        $album = Album::find($invitation->album_id)->with('user')->first();
        $responseData = [
            'album' => [
                'id' => $album->id,
                'name' => $album->name,
                'user' => [
                    'id' => $album->user->id,
                    'name' => $album->user->name,
                    'login' => $album->user->login,
                    'complaint' => $album->user->complaint,
                    'is_banned' => $album->user->is_banned,
                    'role_id' => $album->user->role_id,
                ],
            ],
        ];

        return response()->json($responseData)->setStatusCode(200);
    }
    public function join($code)
    {
        $user = Auth::user();
        $invitation = Invitation::where('link', $code)->first();
        $currentDate = date('Y-m-d H:i:s');
        if ($currentDate > $invitation->expires_at)
        {
            $invitation::where('link', $code)->delete();
            throw new ApiException('Invitation expired', 409);
        }
        if (AlbumAccess::where('user_id', $user->id)->where('album_id', $invitation->album_id)->first()) throw new ApiException('Already exist', 409);
        AlbumAccess::create([
            'user_id' => $user->id,
            'album_id' => $invitation->album_id
        ]);
        if($invitation->join_limit !== null)
        {
            $invitation->join_limit -= 1;
        }
       return response()->json()->setStatusCode(204);
    }
    public function create($album_id, InvitationCreateRequest $request)
    {
        //Проверка принадлежности альбома пользователю
        $user = Auth::user();
        $album = Album::find($album_id);
        if (!$album) throw new ApiException('Not found', 404);
        if($album->user_id != $user->id)
        {
            throw new ApiException('Forbidden', 403);
        }
        if(Invitation::where('album_id', $album_id)->first()) throw new ApiException('Already exist', 418);
        $currentDate = date('Y-m-d H:i:s');

        // Проверка значения expires_at в запросе
        if ($request->expires_at_integer) {
            $date =  strtotime("$currentDate + $request->expires_at_integer minute");
            $formattedDate = date('Y-m-d H:i:s', $date);
        }
        else if ($request->expires_at_date)
        {
            $formattedDate = $request->expires_at_date;
        }
        else {
            $date =  strtotime("$currentDate + 1440 minute");
            $formattedDate = date('Y-m-d H:i:s', $date);
        }

        $invitation = Invitation::create([
            'expires_at' => $formattedDate,
            'album_id' => $album_id,
            'joinLimit' => $request->joinLimit ?? null,
            'link' => Str::random(8)
        ]);

        return response()->json(['invitationCode' => $invitation->link])->setStatusCode(201);
    }
}
