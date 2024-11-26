<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ForbiddenException;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Complaint;
use Closure;
use Illuminate\Http\Request;

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

        // Впускаем админа если есть жалоба
        if ($user->role->code === 'admin') {
            // Проверяем, есть ли жалоба на этого пользователя, помимо отклоненных
            $complaintExists = Complaint
                ::where('about_user_id', $album->user_id)
                ->where('status',null)
                ->where('album_id', $album->id) // Все нерассмотренные жалобы
                ->exists();

            // Если жалоба существует, разрешаем просмотр альбома
            if ($complaintExists)
                return $next($request);
        }

        // Незваных отбрасываем
        throw new ForbiddenException();
    }
}
