<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Invitation;
use Illuminate\Support\Facades\Auth;
use Illuminate\Http\Request;

use Illuminate\Support\Facades\Hash;
use Psy\Util\Str;

class InvitationController extends Controller
{
    public function join($code)
    {
        $user = Auth::user();
        $invitation = Invitation::where('link', $code)->first();
        if(!$invitation) throw new ApiException('Приглашение недействительно', 404);
        AlbumAccess::create([
            'user_id' => $user->id,
            'album_id' => $invitation->album_id
        ]);
        return response()->setStatusCode(204);
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
            'link' => Hash::make($album_id.$formattedDate)
        ]);

        return response()->json(['invitationCode' => $invitation->link])->setStatusCode(201);
    }
}
