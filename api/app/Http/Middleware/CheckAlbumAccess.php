<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ApiException;
use App\Models\Album;
use App\Models\AlbumAccess;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Log;
use Symfony\Component\HttpFoundation\Response;

class CheckAlbumAccess
{

    public function handle(Request $request, Closure $next)
    {
        if($request->isMethod('GET'))
        {
            $user = Auth::user();
            $album_id = $request->route('album');
            Album::findOrFail($album_id);
            $originalAlbum = Album::findOrFail($album_id)->where('user_id', $user->id);
            if($originalAlbum)
            {
                return $next($request);
            }
            $albumAccess = AlbumAccess::where('album_id', $album_id)->where('user_id', $user->id)->first();
            if (!$albumAccess) {throw new ApiException('Forbidden', 403);}
        }


        return $next($request);
    }



}
