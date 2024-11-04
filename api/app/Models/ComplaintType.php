<?php

namespace App\Models;


class ComplaintType extends Model
{
    protected $fillable = [
        'name',
    ];
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }
}
