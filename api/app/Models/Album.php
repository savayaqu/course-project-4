<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Album extends Model
{
    protected $fillable = [
      'name',
      'path'
    ];
    public function pictures()
    {
        return $this->hasMany(Picture::class);
    }
}
