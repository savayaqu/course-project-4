<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Picture extends Model
{
    protected $fillable = [
      'name',
      'hash',
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
    }/*
    public function tags()
    {   // TODO: можно избавится от модели TagPicture, соединив эту модель напрямую к Tag модели
        return $this->belongsToMany(Tag::class, 'tag_pictures');
    }*/
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }
}
