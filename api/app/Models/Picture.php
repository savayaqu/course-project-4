<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Picture extends Model
{
    protected $fillable = [
      'name',
      'path',
      'hash',
      'preview',
      'user_id',
      'album_id'
    ];
    public function album() {
        return $this->belongsTo(Album::class);
    }
    public function user() {
        return $this->belongsTo(User::class);
    }
}
