<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class WarningResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id'      => $this->id,
            'comment' => $this->whenNotNull($this->comment),
            'user'    => $this->whenLoaded('user', fn() => UserResource::make($this->user)),
        ];
    }
}
