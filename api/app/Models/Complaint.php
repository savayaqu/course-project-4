<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Complaint extends Model
{
    protected $fillable = [
      'description',
      'complaint_type_id',
      'picture_id',
      'album_id',
      'user_id'
    ];
    public function complaintType()
    {
        return $this->belongsTo(ComplaintType::class);
    }
    public function picture()
    {
        return $this->belongsTo(Picture::class);
    }
    public function album()
    {
        return $this->belongsTo(Album::class);
    }
    public function user()
    {
        return $this->belongsTo(User::class);
    }
}
