<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ForbiddenException;
use App\Models\Album;
use App\Models\User;
use Closure;
use Exception;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Hash;

class SignCheck
{
    public function handle(Request $request, Closure $next)
    {
        try {

            $sign = $request->query('sign');
            if (!$sign)
                throw new ForbiddenException();

            $albumId = $request->route('album');
            $album = null;
            if ($albumId instanceof Album) {
                $album = $albumId;
                $albumId = $album->id;
            }

            try {
                $signExploded = explode('_', $sign);
                $userId   = $signExploded[0];
                $signCode = $signExploded[1];
            }
            catch (Exception) {
                throw new ForbiddenException();
            }
            $cacheKey = Album::signCacheKey($albumId, $userId);
            $cachedSignRaw = Cache::get($cacheKey);

            $cachedSign = null;
            if ($cachedSignRaw) {
                $cachedSignExploded = explode('_', $cachedSignRaw);
                $cachedSign = $cachedSignExploded[0];
                $ownerId    = $cachedSignExploded[1];
            }

            if ($cachedSign !== $signCode) {
                $user = User::findOrFailCustom($userId);
                if (!$album)
                    $album = Album::findOrFailCustom($albumId);

                $ownerId = $album->user_id;

                $string = Album::signNonHash($user, $albumId);

                try {
                    $allow = Hash::check($string, base64_decode($signCode));
                }
                catch (Exception) {
                    throw new ForbiddenException();
                }

                if ($allow)
                    Cache::put($cacheKey, $signCode . '_' . $ownerId, 3600);
                else
                    throw new ForbiddenException();
            }
            $request->attributes->add(['ownerId' => $ownerId]);
            return $next($request);
        }
        catch (Exception) {
            throw new ForbiddenException();
        }
    }
}
