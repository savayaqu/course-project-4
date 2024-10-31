<?php

namespace App\Models;

// use Illuminate\Contracts\Auth\MustVerifyEmail;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Laravel\Sanctum\HasApiTokens;

class User extends Authenticatable
{
    use HasFactory, Notifiable, HasApiTokens;

    protected $fillable = [
        'name',
        'login',
        'password',
        'role_id',
        'complaint',
        'is_banned'
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
    public function codeRole(array $role)
    {
        return in_array($this->role->code, $role);
    }
    public function role()
    {
        return $this->belongsTo(Role::class);
    }
    public function albums()
    {
        return $this->hasMany(Album::class);
    }
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }
    public function albumAccesses()
    {
        return $this->hasMany(AlbumAccess::class);
    }/*
    public function albumsViaAccess()
    {   // TODO: можно избавится от модели AlbumAccess, соединив эту модель напрямую к Album модели
        return $this->belongsToMany(Album::class, 'album_accesses');
    }*/
    public function tags()
    {
        return $this->hasMany(Tag::class);
    }
    public function warnings()
    {
        return $this->hasMany(Warning::class);
    }

}
