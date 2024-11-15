<?php

namespace App\Models;

// use Illuminate\Contracts\Auth\MustVerifyEmail;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Auth\Authenticatable;
use Illuminate\Auth\MustVerifyEmail;
use Illuminate\Auth\Passwords\CanResetPassword;
use Illuminate\Contracts\Auth\Access\Authorizable as AuthorizableContract;
use Illuminate\Contracts\Auth\Authenticatable as AuthenticatableContract;
use Illuminate\Contracts\Auth\CanResetPassword as CanResetPasswordContract;
use Illuminate\Foundation\Auth\Access\Authorizable;
use Illuminate\Notifications\Notifiable;
use Laravel\Sanctum\HasApiTokens;

class User extends Model implements
    AuthenticatableContract,
    AuthorizableContract,
    CanResetPasswordContract
{
    use HasFactory, Notifiable, HasApiTokens, Authenticatable, Authorizable, CanResetPassword, MustVerifyEmail;

    protected $fillable = [
        'name',
        'login',
        'password',
        'role_id',
        'is_banned', // TODO: Поле блокировки есть, а проверки нет
    ];

    protected $hidden = [
        'password',
        'remember_token',
    ];

    protected function casts(): array
    {
        return [
            'password' => 'hashed',
        ];
    }

    public function role() {
        return $this->belongsTo(Role::class);
    }
    public function albums() {
        return $this->hasMany(Album::class);
    }
    public function pictures() {
        return $this->hasManyThrough(Album::class, Picture::class, 'album_id', 'user_id');
    }
    public function complaintsFrom() {
        return $this->hasMany(Complaint::class, 'from_user_id');
    }
    public function complaintsAbout() {
        return $this->hasMany(Complaint::class, 'about_user_id');
    }
    public function albumAccesses() {
        return $this->hasMany(AlbumAccess::class);
    }
    public function albumsViaAccess() {
        return $this->belongsToMany(Album::class, 'album_accesses')->using(AlbumAccess::class);
    }
    public function tags() {
        return $this->hasMany(Tag::class);
    }
    public function warnings() {
        return $this->hasMany(Warning::class);
    }
}
