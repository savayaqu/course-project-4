<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Album extends Model
{
    protected $fillable = [
      'name',
      'path',
      'user_id'
    ];
    public function pictures()
    {
        return $this->hasMany(Picture::class);
    }
    public function user() {
        return $this->belongsTo(User::class);
    }
}
