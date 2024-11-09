<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;
use Illuminate\Support\Facades\Auth;

class InvitationResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $array = [];
        if ($this->album->user_id === Auth::user()->id) {
            $array['expires_at'] = $this->expires_at;
            $array['join_limit'] = $this->join_limit;
        }
        $array['album'] = AlbumResource::make($this->album);
        return $array;
    }
}
