<?php

namespace App\Models;

class Role extends Model
{
    protected $fillable = ['code'];

    public function users() {
        return $this->hasMany(User::class);
    }
}
