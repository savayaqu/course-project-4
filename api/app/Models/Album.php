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
    public function user() {
        return $this->belongsTo(User::class);
    }
    public function pictures()
    {
        return $this->hasMany(Picture::class);
    }
    public function albumAccesses()
    {
        return $this->hasMany(AlbumAccess::class);
    }
    public function invitations()
    {
        return $this->hasMany(Invitation::class);
    }
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }

}
