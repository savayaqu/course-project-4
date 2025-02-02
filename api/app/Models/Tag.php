<?php

namespace App\Models;

class Tag extends Model
{
    // Поля
    protected $fillable = [
        'value',
        'user_id',
    ];

    // Связи
    public function user() {
        return $this->belongsTo(User::class);
    }
    public function tagPictures() {
        return $this->hasMany(TagPicture::class);
    }
    public function pictures() {
        return $this->belongsToMany(Picture::class, TagPicture::class)->using(TagPicture::class);
    }
}
