<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Picture extends Model
{
    protected $fillable = [
      'name',
      'date',
      'size',
      'width',
      'height',
      'album_id'
    ];
    public function user() {
        return $this->belongsTo(User::class);
    }
}
