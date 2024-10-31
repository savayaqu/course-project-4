<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Crypt;
use Illuminate\Support\Facades\Hash;

class Album extends Model
{
    protected $fillable = [
      'name',
      'path',
      'user_id'
    ];
    public function user() {
        return $this->belongsTo(User::class);
    }
    public function pictures()
    {
        return $this->hasMany(Picture::class);
    }
    public function albumAccesses()
    {
        return $this->hasMany(AlbumAccess::class);
    }/*
    public function usersViaAccess()
    {   // TODO: можно избавится от модели AlbumAccess, соединив эту модель напрямую к User модели
        return $this->belongsToMany(User::class, 'album_accesses');
    }*/
    public function invitations()
    {
        return $this->hasMany(Invitation::class);
    }
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }

    public static function getSign(User $user, $albumId): string {
        $cacheKey = "signAccess:to=$albumId;for=$user->id";
        $cachedSign = Cache::get($cacheKey);
        if ($cachedSign) return $user->id .'_'. $cachedSign;

        $currentDay = date("Y-m-d");
        $userToken = $user->getRememberToken();

        $string = $userToken . $currentDay . $albumId;
        $signCode = base64_encode(Hash::make($string));

        Cache::put($cacheKey, $signCode, 3600);
        return $user->id .'_'. $signCode;
    }

    public static function checkSign($albumId, $sign)
    {
        try {
            $signExploded = explode('_', $sign);
            $userId   = $signExploded[0];
            $signCode = $signExploded[1];
        }
        catch (\Exception $e) {
            return false;
        }

        $cacheKey = "signAccess:to=$albumId;for=$userId";
        $cachedSign = Cache::get("signAccess:to=$albumId;for=$userId");
        if ($cachedSign !== $signCode) return false;

        $user = User::find($signExploded[0]);
        if (!$user)
            return false;

        $currentDay = date("Y-m-d");
        $string = $user->getRememberToken() . $currentDay . $albumId;

        $allow = Hash::check($string, base64_decode($signCode));
        if ($allow)
            Cache::put($cacheKey, $signCode, 3600);

        return $user->login;
    }
}
