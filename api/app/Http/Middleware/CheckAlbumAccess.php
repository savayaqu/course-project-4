<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ForbiddenException;
use App\Models\Album;
use App\Models\AlbumAccess;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Gate;

class CheckAlbumAccess
{
    public function handle(Request $request, Closure $next)
    {
        $user = $request->user();
        $album = $request->route('album');
        if (!($album instanceof Album))
            $album = Album::findOrFailCustom($album);

        // Впускаем владельца
        if ($album->user_id === $user->id)
            return $next($request);

        // Впускаем по выданному владельцем доступу на чтение
        if (
            $request->isMethod('GET') &&
            AlbumAccess
            ::where('album_id', $album->id)
            ->where('user_id' , $user->id)
            ->exists()
        ) return $next($request);
        // Проверяем доступ через политику
        if (Gate::allows('view-album', $album)) {
            return $next($request);
        }
        // Незваных отбрасываем
        throw new ForbiddenException();
    }
}
