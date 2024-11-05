<?php

namespace App\Http\Resources;

use Illuminate\Container\Attributes\Auth;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class UserPublicResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $array = [
            'id'   => $this->id,
            'name' => $this->name,
        ];

        return $array;
    }
}
