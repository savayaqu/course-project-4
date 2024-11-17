<?php

namespace App\Models;

use App\Exceptions\Api\ApiException;

class Invitation extends Model
{
    protected $fillable = [
        'link',
        'expires_at',
        'album_id',
        'join_limit',
    ];

    public function getRouteKeyName() {
        return 'link';
    }

    public function failOnExpires(): void
    {
        if (now()->greaterThan($this->expires_at)) {
            $this->delete();
            throw new ApiException('Invitation expired', 409);
        }
        if ($this->join_limit !== null &&
            $this->join_limit < 1) {
            $this->delete();
            throw new ApiException('Invitation expired', 409);
        }
    }

    public function album() {
        return $this->belongsTo(Album::class);
    }
}
