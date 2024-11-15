<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Exceptions\Api\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Complaint\ComplaintCreateRequest;
use App\Http\Resources\ComplaintResource;
use App\Models\Album;
use App\Models\Complaint;
use App\Models\Picture;
use Illuminate\Support\Facades\Auth;

class ComplaintController extends Controller
{
    public function index()
    {
        $user = Auth::user();
        $query = Complaint::with(['type', 'aboutUser', 'fromUser', 'picture', 'album']);

        if ($user->role->code !== 'admin')
            $query = Complaint::where('from_user_id', $user->id);

        return response(['complaints' => ComplaintResource::collection($query->get())]);
    }

    public function createToPicture(ComplaintCreateRequest $request, Album $album, Picture $picture)
    {
        $user = Auth::user();
        $isAccessible = $album->usersViaAccess()->where('user_id', $user->id)->exists();
        if(!$isAccessible)
            throw new ForbiddenException();

        $isExisted = Complaint
            ::where('from_user_id', $user->id)
            ->where('picture_id', $picture->id)
            ->exists();
        if ($isExisted)
            throw new ApiException('You are already complain to this picture', 409);

        Complaint::create([
            'picture_id'        => $picture->id,
            'description'       => $request->description,
            'complaint_type_id' => $request->typeId,
            'from_user_id'      => $user->id,
            'about_user_id'     => $album->user_id,
        ]);
        return response(null, 204);
    }

    public function createToAlbum(ComplaintCreateRequest $request, Album $album)
    {
        $user = Auth::user();
        $isAccessible = $album->usersViaAccess()->where('user_id', $user->id)->exists();
        if(!$isAccessible)
            throw new ForbiddenException();

        $isExisted = Complaint
            ::where('from_user_id', $user->id)
            ->where('album_id', $album->id)
            ->exists();
        if($isExisted)
            throw new ApiException('You are already complain to this album', 409);

        Complaint::create([
            'album_id'          => $album->id,
            'description'       => $request->description,
            'complaint_type_id' => $request->typeId,
            'from_user_id'      => $user->id,
            'about_user_id'     => $album->user_id,
        ]);
        return response(null, 204);
    }

    public function destroy(Complaint $complaint)
    {
        $user = Auth::user();
        if ($complaint->from_user_id !== $user->id)
            throw new ForbiddenException();

        $complaint->delete();
        return response(null, 204);
    }
}
