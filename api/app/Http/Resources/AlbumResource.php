<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;
use Illuminate\Support\Facades\Auth;

class AlbumResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $array = [
            'id'   => $this->id,
            'name' => $this->name,
            'pictures' => $this->pictures()->take(4)->get()
        ];
        if ($this->user_id !== Auth::user()->id) {
            $array['owner'] = UserPublicResource::make($this->user);
        }
        else {
            $array['path'] = $this->path;
            $array['created_at'] = $this->created_at;
        }
        return $array;
    }
}
