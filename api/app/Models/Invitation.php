<?php

namespace App\Models;


class Invitation extends Model
{
    protected $fillable = [
        'link',
        'expires_at',
        'album_id',
        'join_limit'
    ];
    public function album()
    {
        return $this->belongsTo(Album::class);
    }
}
