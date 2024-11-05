<?php

namespace App\Models;

class Tag extends Model
{
    protected $fillable = [
      'value',
      'user_id',
    ];
    public function user()
    {
        return $this->belongsTo(User::class);
    }
    public function tagPictures()
    {
        return $this->hasMany(TagPicture::class);
    }
}
