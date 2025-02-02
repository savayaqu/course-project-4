<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Relations\Pivot;

class AlbumAccess extends Pivot
{
    protected $table = 'album_accesses';

    // Поля
    protected $fillable = [
        'album_id',
        'user_id',
    ];

    // Связи
    public function album() {
        return $this->belongsTo(Album::class);
    }
    public function user() {
        return $this->belongsTo(User::class);
    }
}
