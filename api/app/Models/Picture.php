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
    public function album() {
        return $this->belongsTo(Album::class);
    }
    public function tagPictures()
    {
        return $this->hasMany(TagPicture::class);
    }
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }
}
