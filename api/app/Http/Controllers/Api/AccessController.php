<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\AlbumAccess;
use App\Models\Invitation;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class AccessController extends Controller
{
    public function all()
    {
        $user = Auth::user();
        // поиск всех ссылок от текущего пользователя
        $invitations = Invitation::with('album')->whereHas('album', function ($query) use ($user) {
            $query->where('user_id', $user->id);
        })->get();

        //Получаем все album_id из приглашений
        $albumIds = $invitations->pluck('album.id')->toArray();

        //поиск всех пользователей у которых есть доступ к альбомам текущего пользователя
       $accesses = AlbumAccess::whereIn('album_id', $albumIds)->get();

       return response()->json(['accesses' => $accesses, 'invitations' => $invitations]);

        // TODO:: это пиздец, а не вывод
    }
    public function destroy($albumId, $userId)
    {
        $albumAccess = AlbumAccess::where('album_id', $albumId)->where('user_id', $userId)->delete();
        if(!$albumAccess) throw new ApiException('Не найдено', 404);
        return response()->json()->setStatusCode(204);
    }
}
