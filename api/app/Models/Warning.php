<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Warning extends Model
{
    protected $fillable = ['comment', 'user_id'];

    public function user()
    {
        return $this->belongsTo(User::class);
    }
}
