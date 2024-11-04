<?php

namespace App\Models;


class Invitation extends Model
{
    protected $fillable = [
      'link',
      'expires_at',
      'album_id'
    ];
    public function album()
    {
        return $this->belongsTo(Album::class);
    }
}
