<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ApiException;
use App\Models\Album;
use App\Models\User;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Hash;
use Symfony\Component\HttpFoundation\Response;

class SignCheck
{

    public function handle(Request $request, Closure $next)
    {
        $albumId = $request->route('album');
        $sign = $request->query('sign');
        try {
            $signExploded = explode('_', $sign);
            $userId   = $signExploded[0];
            $signCode = $signExploded[1];
        }
        catch (\Exception $e) {
            throw new ApiException('Forbidden', 403);
        }

        $cacheKey = "signAccess:to=$albumId;for=$userId";
        $cachedSign = Cache::get("signAccess:to=$albumId;for=$userId");
        if ($cachedSign !== $signCode) throw new ApiException('Forbidden', 403);

        $user = User::findOrFailCustom($signExploded[0]);
        $originalAlbum = Album::findOrFailCustom($albumId);
        $originalUser = User::findOrFailCustom($originalAlbum->user_id);
        $currentDay = date("Y-m-d");
        $string = $user->getRememberToken() . $currentDay . $albumId;

        $allow = Hash::check($string, base64_decode($signCode));
        if ($allow)
            Cache::put($cacheKey, $signCode, 3600);
        //Записываем в реквест логин пользователя (автора альбома)
        $request->login = $originalUser->login;
        return $next($request);
    }
}
