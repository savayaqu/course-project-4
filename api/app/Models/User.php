<?php

namespace App\Models;

// use Illuminate\Contracts\Auth\MustVerifyEmail;
use App\Cacheables\SpaceInfo;
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

    // Поля
    protected $fillable = [
        'name',
        'login',
        'password',
        'role_id',
        'is_banned',
    ];

    // Скрытые
    protected $hidden = [
        'password',
        'remember_token',
    ];

    // Установка по умолчанию
    protected $attributes = [
        'is_banned' => false,
    ];

    // Модификация полей
    protected function casts(): array
    {
        return [
            'password' => 'hashed',
            'is_banned' => 'boolean'
        ];
    }

    // Функции
    public function ban(): void
    {
        AlbumAccess::whereIn('album_id', $this->albums()->pluck('id'))->delete();
        Complaint::where('about_user_id', $this->id)->update(['status' => 1]);
        $this->tokens()->delete();
    }

    public function quotaTotal(): int
    {
        if ($this->role->name == 'admin') {
            $space = SpaceInfo::get();
            $usedHim = $this->quotaUsed();
            return $space->total - ($space->used - $usedHim);
        }
        else {
            return min(
                config('settings.free_storage_limit'),
                SpaceInfo::getCached()->free
            );
        }
    }

    public function quotaUsed(): int
    {
        return $this->pictures()->sum('size');
    }

    // Связи
    public function role() {
        return $this->belongsTo(Role::class);
    }
    public function albums() {
        return $this->hasMany(Album::class);
    }
    public function pictures() {
        return $this->hasManyThrough(
            Picture::class,
            Album::class,
            'user_id',
            'album_id'
        );
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
        return $this->belongsToMany(Album::class, AlbumAccess::class)
            ->using(AlbumAccess::class);
    }
    public function tags() {
        return $this->hasMany(Tag::class);
    }
    public function warnings() {
        return $this->hasMany(Warning::class);
    }
}
