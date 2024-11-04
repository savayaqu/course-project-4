<?php

namespace App\Models;

class Warning extends Model
{
    protected $fillable = ['comment', 'user_id'];

    public function user()
    {
        return $this->belongsTo(User::class);
    }
}
