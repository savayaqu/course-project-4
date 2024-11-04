<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Middleware\CheckRole;
use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Complaint;
use App\Models\Picture;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class ComplaintController extends Controller
{
    public function destroy($complaintId)
    {
        $user = Auth::user();
        Complaint::findOrFail($complaintId)->where('from_user_id', $user->id)->delete();
        return response()->json()->setStatusCode(204);
    }
    public function all()
    {
        $user = Auth::user()->with('role')->first();
        if($user->role->code == 'admin')
        {
            $complaints = Complaint::with(['user', 'aboutUser'])->get();
            return response()->json($complaints)->setStatusCode(200);
        }
        return response()->json(Complaint::where('from_user_id', $user->id)->get())->setStatusCode(200);
    }
    public function createToPicture(Request $request, $albumId, $pictureId)
    {
        $user = Auth::user();
        $album = Album::findOrFail($albumId);
        if(!AlbumAccess::where('user_id', $user->id)->where('album_id', $albumId)->exists()) {
            throw new ApiException('Forbidden', 403);
        }
        Picture::findOrFail($pictureId);
        if(Complaint::where('from_user_id', $user->id)->where('picture_id', $pictureId)->exists()) {
            throw new ApiException('You are already complain to this picture', 409);
        }
        Complaint::create([
            'picture_id' => $pictureId,
            'description' => $request->description,
            'complaint_type_id' => $request->type_id,
            'from_user_id' => $user->id,
            'about_user_id' => $album->user_id,
        ]);
        return response()->json()->setStatusCode(204);
    }

    public function createToAlbum(Request $request, $albumId)
    {
        $user = Auth::user();

        $album = Album::find($albumId);
        if(!$album) {throw new ApiException('Not found', 404);}
        if(!AlbumAccess::where('user_id', $user->id)->where('album_id', $albumId)->exists()) {
            throw new ApiException('Forbidden', 403);
        }
        if(Complaint::where('from_user_id', $user->id)->where('album_id', $albumId)->exists()) {
            throw new ApiException('You are already complain to this album', 409);
        }
        Complaint::create([
           'album_id' => $albumId,
           'description' => $request->description,
           'complaint_type_id' => $request->type_id,
            'from_user_id' => $user->id,
            'about_user_id' => $album->user_id,
        ]);
        return response()->json()->setStatusCode(204);
    }
}
