<?php

namespace App\Models;

class TagPicture extends Model
{
    protected $fillable = [
      'tag_id',
      'picture_id',
    ];
    public function tag()
    {
        return $this->belongsTo(Tag::class);
    }
    public function picture()
    {
        return $this->belongsTo(Picture::class);
    }
}
