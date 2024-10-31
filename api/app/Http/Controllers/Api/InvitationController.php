<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Invitation;
use Illuminate\Support\Facades\Auth;
use Illuminate\Http\Request;

use Illuminate\Support\Str;

class InvitationController extends Controller
{

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
        if(!$invitation) throw new ApiException('Приглашение недействительно', 404);
        if(AlbumAccess::where('user_id', $user->id)->where('album_id', $invitation->album_id)->first()) throw new ApiException('Вы уже имеете доступ', 404);
        AlbumAccess::create([
            'user_id' => $user->id,
            'album_id' => $invitation->album_id
        ]);
        return response()->json()->setStatusCode(204);
    }
    public function create($album_id, Request $request)
    {
        $album = Album::find($album_id);
        if (!$album) throw new ApiException('Альбом не найден', 404);
        if(Invitation::where('album_id', $album_id)->first()) throw new ApiException('Приглашение уже существует', 418);
        $currentDate = date('Y-m-d H:i:s');

        // Проверка значения expires_at в запросе
        if (empty($request->expires_at)) {
            $date =  strtotime("$currentDate + $request->expires_at minute");
            $formattedDate = date('Y-m-d H:i:s', $date);

        } else {
            $date =  strtotime("$currentDate + 1440 minute");
            $formattedDate = date('Y-m-d H:i:s', $date);
        }

        $invitation = Invitation::create([
            'expires_at' => $formattedDate,
            'album_id' => $album_id,
            'link' => Str::random(8)
        ]);

        return response()->json(['invitationCode' => $invitation->link])->setStatusCode(201);
    }
}
