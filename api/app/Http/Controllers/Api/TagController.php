<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Tag\TagCreateRequest;
use App\Http\Requests\Api\Tag\TagUpdateRequest;
use App\Models\Album;
use App\Models\Picture;
use App\Models\Tag;
use App\Models\TagPicture;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class TagController extends Controller
{
    public function attachToPicture($albumId, $pictureId, $tagId)
    {
        $user = Auth::user();
        $album = Album::where('id', $albumId)->where('user_id', $user->id)->first();
        if(!$album) {throw  new ApiException('Not found', 404);}
        $picture = Picture::where('id', $pictureId);
        if(!$picture) {throw  new ApiException('Not found', 404);}
        $tag = Tag::where('id', $tagId)->where('user_id', $user->id)->first();
        if(!$tag) {throw  new ApiException('Not found', 404);}
        TagPicture::create([
           'picture_id' => $pictureId,
           'tag_id' => $tagId
        ]);
        return response()->json()->setStatusCode(204);
    }
    public function detachToPicture($albumId, $pictureId, $tagId)
    {
        $user = Auth::user();
        $album = Album::where('id', $albumId)->where('user_id', $user->id)->first();
        if(!$album) {throw  new ApiException('Not found', 404);}
        $picture = Picture::where('id', $pictureId);
        if(!$picture) {throw  new ApiException('Not found', 404);}
        $tag = Tag::where('id', $tagId)->where('user_id', $user->id)->first();
        if(!$tag) {throw  new ApiException('Not found', 404);}
        TagPicture::where('picture_id', $pictureId)->where('tag_id', $tagId)->delete();
        return response()->json()->setStatusCode(204);
    }
    public function create(TagCreateRequest $request)
    {
        $user = Auth::user();
        if(Tag::where('value', $request->value)->first())
        {
            throw new ApiException('Already exists', 409);
        }
        $tag = Tag::create([
           'value' => $request->value,
           'user_id' => $user->id
        ]);
        return response()->json(['id' => $tag->id,'name' => $tag->value])->setStatusCode(201);
    }
    public function index()
    {
        $user =Auth::user();
        $tags = Tag::where('user_id', $user->id)->get();
        return response()->json($tags)->setStatusCode(200);
    }
    public function show($tagId)
    {
        $user = Auth::user();
        if(!Tag::find($tagId)->where('user_id', $user->id)->first()) {throw new ApiException('Not found', 404);}
        $tagPictures = TagPicture::where('tag_id', $tagId)->with('picture')->get();
        return response()->json($tagPictures)->setStatusCode(200);
    }
    public function edit($tagId, TagUpdateRequest $request)
    {
        Tag::findOrFail($tagId)->update($request->all());
        return response()->json(Tag::findOrFail($tagId))->setStatusCode(200);
    }
    public function destroy($tagId)
    {
        $user = Auth::user();

        $tag = Tag::where('id', $tagId)->where('user_id', $user->id)->delete();
        if(!$tag) {
            throw new ApiException('Not found', 404);
        }
        return response()->json()->setStatusCode(204);
    }
    //
}
