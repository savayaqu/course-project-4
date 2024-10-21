<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AlbumAccess extends Model
{
    protected $fillable = [
        'album_id',
        'user_id',
    ];
    public function album()
    {
        return $this->belongsTo(Album::class);
    }
    public function user()
    {
        return $this->belongsTo(User::class);
    }
}
