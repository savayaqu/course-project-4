<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;
use Illuminate\Support\Facades\Auth;

class InvitationResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $array = [
            'code'      => $this->link,
            'expiresAt' => $this->expires_at,
            'joinLimit' => $this->join_limit,
        ];
        //if ($this->album->user_id === Auth::user()->id) {
        //    $array['expiresAt'] = $this->expires_at;
        //    $array['joinLimit'] = $this->join_limit;
        //}
        //if ($this->whenLoaded('album'))
        //    $array['album'] = AlbumResource::make($this->album);
        return $array;
    }
}
